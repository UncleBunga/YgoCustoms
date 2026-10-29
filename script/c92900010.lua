-- Slasher - Doll Heir
local s,id=GetID()
function s.initial_effect(c)
    c:EnableCounterPermit(0x199)
    -- Xyz Summon Procedure (2+ Level 2 monsters)
    c:EnableReviveLimit()
    Xyz.AddProcedure(c,nil,2,2,nil,nil,Xyz.InfiniteMats)

    -- Once per turn: target 1 monster in either GY; attach it to this card as material
    local e1=Effect.CreateEffect(c)
    e1:SetDescription(aux.Stringid(id,0))
    e1:SetType(EFFECT_TYPE_IGNITION)
    e1:SetProperty(EFFECT_FLAG_CARD_TARGET)
    e1:SetRange(LOCATION_MZONE)
    e1:SetCountLimit(1)
    e1:SetTarget(s.atttg)
    e1:SetOperation(s.attop)
    c:RegisterEffect(e1)

    -- Quick Effect: when opponent activates a card or effect, change position; if you do, detach 1 and apply
    local e2=Effect.CreateEffect(c)
    e2:SetDescription(aux.Stringid(id,1))
    e2:SetCategory(CATEGORY_POSITION+CATEGORY_DESTROY)
    e2:SetType(EFFECT_TYPE_QUICK_O)
    e2:SetCode(EVENT_CHAINING)
    e2:SetProperty(EFFECT_FLAG_DAMAGE_STEP+EFFECT_FLAG_DAMAGE_CAL)
    e2:SetRange(LOCATION_MZONE)
    e2:SetCondition(s.poscon)
    e2:SetTarget(s.postg)
    e2:SetOperation(s.posop)
    c:RegisterEffect(e2)
end
s.counter_list={0x199}

function s.attfilter(c,xc,tp)
    return c:IsMonster() and c:IsCanBeXyzMaterial(xc,tp,REASON_EFFECT)
end

function s.atttg(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
    local c=e:GetHandler()
    if chkc then return chkc:IsLocation(LOCATION_GRAVE) and s.attfilter(chkc,c,tp) end
    if chk==0 then return c:IsType(TYPE_XYZ)
        and Duel.IsExistingTarget(s.attfilter,tp,LOCATION_GRAVE,LOCATION_GRAVE,1,nil,c,tp) end
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_XMATERIAL)
    local g=Duel.SelectTarget(tp,s.attfilter,tp,LOCATION_GRAVE,LOCATION_GRAVE,1,1,nil,c,tp)
    Duel.SetOperationInfo(0,CATEGORY_LEAVE_GRAVE,g,1,0,0)
end

function s.attop(e,tp,eg,ep,ev,re,r,rp)
    local c=e:GetHandler()
    local tc=Duel.GetFirstTarget()
    if c:IsRelateToEffect(e) and c:IsFaceup() and tc and tc:IsRelateToEffect(e) and not tc:IsImmuneToEffect(e) then
        Duel.Overlay(c,tc)
    end
end

function s.poscon(e,tp,eg,ep,ev,re,r,rp)
    return rp==1-tp
end

function s.postg(e,tp,eg,ep,ev,re,r,rp,chk)
    local c=e:GetHandler()
    if chk==0 then return c:IsCanChangePosition() end
    Duel.SetOperationInfo(0,CATEGORY_POSITION,c,1,0,0)
end

function s.posop(e,tp,eg,ep,ev,re,r,rp)
    local c=e:GetHandler()
    if not (c:IsRelateToEffect(e) and c:IsFaceup()) then return end
    if Duel.ChangePosition(c,POS_FACEUP_DEFENSE,POS_FACEUP_DEFENSE,POS_FACEUP_ATTACK,POS_FACEUP_ATTACK)==0 then return end
    if not c:CheckRemoveOverlayCard(tp,1,REASON_EFFECT) then return end
    if c:RemoveOverlayCard(tp,1,1,REASON_EFFECT)==0 then return end
    Duel.BreakEffect()
    if c:IsAttackPos() then
        -- Attack Position: destroy 1 card on the field
        Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_DESTROY)
        local g=Duel.SelectMatchingCard(tp,nil,tp,LOCATION_ONFIELD,LOCATION_ONFIELD,1,1,nil)
        if #g>0 then
            Duel.HintSelection(g)
            Duel.Destroy(g,REASON_EFFECT)
        end
    else
        -- Defense Position: "Slasher" cards you control cannot be targeted by opponent's card effects this turn
        local e1=Effect.CreateEffect(c)
        e1:SetType(EFFECT_TYPE_FIELD)
        e1:SetCode(EFFECT_CANNOT_BE_EFFECT_TARGET)
        e1:SetTargetRange(LOCATION_ONFIELD,0)
        e1:SetTarget(aux.TargetBoolFunction(Card.IsSetCard,0x991))
        e1:SetValue(aux.tgoval)
        e1:SetReset(RESET_PHASE+PHASE_END)
        Duel.RegisterEffect(e1,tp)
    end
end
