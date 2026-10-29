--Slasher - Cursed Videotape
local s,id=GetID()
local WELL_DWELLER_ID = 92900036 -- Change this to match your "Slasher - Well Dweller" ID

function s.initial_effect(c)
    c:EnableCounterPermit(0x199)

    -- Activate: Equip to an opponent's face-up monster
    local e1=Effect.CreateEffect(c)
    e1:SetCategory(CATEGORY_EQUIP)
    e1:SetType(EFFECT_TYPE_ACTIVATE)
    e1:SetCode(EVENT_FREE_CHAIN)
    e1:SetProperty(EFFECT_FLAG_CARD_TARGET)
    e1:SetTarget(s.target)
    e1:SetOperation(s.activate)
    c:RegisterEffect(e1)

    -- Standby Phase: Place 1 Kill Counter on this card
    local e2=Effect.CreateEffect(c)
    e2:SetDescription(aux.Stringid(id,0))
    e2:SetCategory(CATEGORY_COUNTER)
    e2:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_TRIGGER_F)
    e2:SetCode(EVENT_PHASE+PHASE_STANDBY)
    e2:SetRange(LOCATION_SZONE)
    e2:SetCountLimit(1)
    e2:SetOperation(s.ctop)
    c:RegisterEffect(e2)

    -- Quick Effect: Banish this card with 3+ Kill Counters to Special Summon "Slasher - Well Dweller"
    local e3=Effect.CreateEffect(c)
    e3:SetDescription(aux.Stringid(id,1))
    e3:SetCategory(CATEGORY_SPECIAL_SUMMON)
    e3:SetType(EFFECT_TYPE_QUICK_O)
    e3:SetCode(EVENT_FREE_CHAIN)
    e3:SetRange(LOCATION_SZONE)
    e3:SetHintTiming(0,TIMINGS_CHECK_MONSTER+TIMING_MAIN_END)
    e3:SetCondition(s.spcon)
    e3:SetCost(s.spcost)
    e3:SetTarget(s.sptg)
    e3:SetOperation(s.spop)
    c:RegisterEffect(e3)

    -- Re-equip to "that Summoned monster" if equipped monster is Tributed or used as material
    local e4=Effect.CreateEffect(c)
    e4:SetDescription(aux.Stringid(id,2))
    e4:SetCategory(CATEGORY_EQUIP)
    e4:SetType(EFFECT_TYPE_SINGLE+EFFECT_TYPE_TRIGGER_O)
    e4:SetProperty(EFFECT_FLAG_DELAY+EFFECT_FLAG_CARD_TARGET)
    e4:SetCode(EVENT_TO_GRAVE)
    e4:SetCondition(s.reqcon)
    e4:SetTarget(s.reqtg)
    e4:SetOperation(s.reqop)
    c:RegisterEffect(e4)
end

-- Activation target: opponent's face-up monster
function s.target(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
    if chkc then return chkc:IsLocation(LOCATION_MZONE) and chkc:IsControler(1-tp) and chkc:IsFaceup() end
    if chk==0 then return Duel.IsExistingTarget(Card.IsFaceup,tp,0,LOCATION_MZONE,1,nil) end
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_EQUIP)
    Duel.SelectTarget(tp,Card.IsFaceup,tp,0,LOCATION_MZONE,1,1,nil)
    Duel.SetOperationInfo(0,CATEGORY_EQUIP,e:GetHandler(),1,0,0)
end

function s.activate(e,tp,eg,ep,ev,re,r,rp)
    local c=e:GetHandler()
    local tc=Duel.GetFirstTarget()
    if c:IsRelateToEffect(e) and tc and tc:IsRelateToEffect(e) and tc:IsFaceup() then
        if Duel.Equip(tp,c,tc) then
            -- Ongoing restriction, registered only now that the equip actually succeeded
            local e0=Effect.CreateEffect(c)
            e0:SetType(EFFECT_TYPE_SINGLE)
            e0:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
            e0:SetCode(EFFECT_EQUIP_LIMIT)
            e0:SetReset(RESET_EVENT+RESETS_STANDARD)
            e0:SetValue(s.eqlimit)
            c:RegisterEffect(e0)
        end
    end
end

-- Ongoing check: equipped monster must remain an opponent's face-up monster
function s.eqlimit(e,c)
    return c:IsControler(1-e:GetHandlerPlayer()) and c:IsFaceup()
end

-- Add Kill Counter during Standby Phase
function s.ctop(e,tp,eg,ep,ev,re,r,rp)
    local c=e:GetHandler()
    if c:IsRelateToEffect(e) and c:IsFaceup() then
        c:AddCounter(0x199,1)
    end
end

-- Check for 3 or more Kill Counters
function s.spcon(e,tp,eg,ep,ev,re,r,rp)
    return e:GetHandler():GetCounter(0x199)>=3
end

function s.spcost(e,tp,eg,ep,ev,re,r,rp,chk)
    local c=e:GetHandler()
    if chk==0 then return c:IsAbleToRemoveAsCost() end
    Duel.Remove(c,POS_FACEUP,REASON_COST)
end

-- Filter for "Slasher - Well Dweller"
function s.spfilter(c,e,tp)
    return c:IsCode(WELL_DWELLER_ID) and c:IsCanBeSpecialSummoned(e,0,tp,true,false)
end

function s.sptg(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return Duel.GetLocationCount(tp,LOCATION_MZONE)>0
        and Duel.IsExistingMatchingCard(s.spfilter,tp,LOCATION_HAND+LOCATION_DECK+LOCATION_GRAVE,0,1,nil,e,tp) end
    Duel.SetOperationInfo(0,CATEGORY_SPECIAL_SUMMON,nil,1,tp,LOCATION_HAND+LOCATION_DECK+LOCATION_GRAVE)
end

function s.spop(e,tp,eg,ep,ev,re,r,rp)
    if Duel.GetLocationCount(tp,LOCATION_MZONE)<=0 then return end
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_SPSUMMON)
    local g=Duel.SelectMatchingCard(tp,aux.NecroValleyFilter(s.spfilter),tp,LOCATION_HAND+LOCATION_DECK+LOCATION_GRAVE,0,1,1,nil,e,tp)
    if #g>0 then
        Duel.SpecialSummon(g,0,tp,tp,true,false,POS_FACEUP)
    end
end

-- Re-equip condition: check if equipped monster was Tributed or used as Material
function s.reqcon(e,tp,eg,ep,ev,re,r,rp)
    local c=e:GetHandler()
    local ec=c:GetPreviousEquipTarget()
    if c:IsReason(REASON_LOST_TARGET) and ec and ec:IsReason(REASON_SYNCHRO+REASON_FUSION+REASON_XYZ+REASON_LINK+REASON_SUMMON) then
        e:SetLabelObject(ec:GetReasonCard())
        return true
    end
    return false
end

function s.reqtg(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
    local tc=e:GetLabelObject()
    if chkc then return chkc==tc end
    if chk==0 then return tc and tc:IsCanBeEffectTarget(e) end
    Duel.SetTargetCard(tc)
    Duel.SetOperationInfo(0,CATEGORY_EQUIP,e:GetHandler(),1,0,0)
end

function s.reqop(e,tp,eg,ep,ev,re,r,rp)
    local c=e:GetHandler()
    local tc=Duel.GetFirstTarget()
    if c:IsRelateToEffect(e) and tc and tc:IsRelateToEffect(e) and tc:IsFaceup() then
        if Duel.Equip(tp,c,tc) then
            local e0=Effect.CreateEffect(c)
            e0:SetType(EFFECT_TYPE_SINGLE)
            e0:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
            e0:SetCode(EFFECT_EQUIP_LIMIT)
            e0:SetReset(RESET_EVENT+RESETS_STANDARD)
            e0:SetValue(s.eqlimit)
            c:RegisterEffect(e0)
        end
    end
end