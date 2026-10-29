-- Slasher - Brittle Blessing
local s,id=GetID()
function s.initial_effect(c)
    -- Activate: Target 1 monster in opponent's GY, opponent chooses the effect
    local e1=Effect.CreateEffect(c)
    e1:SetCategory(CATEGORY_SPECIAL_SUMMON+CATEGORY_RECOVER+CATEGORY_REMOVE)
    e1:SetType(EFFECT_TYPE_ACTIVATE)
    e1:SetProperty(EFFECT_FLAG_CARD_TARGET)
    e1:SetCode(EVENT_FREE_CHAIN)
    e1:SetCountLimit(1,id,EFFECT_COUNT_CODE_OATH)
    e1:SetTarget(s.target)
    e1:SetOperation(s.activate)
    c:RegisterEffect(e1)
end

function s.filter(c,e,tp)
    return c:IsMonster() and (c:IsCanBeSpecialSummoned(e,0,tp,false,false) or c:IsAbleToRemove())
end

function s.target(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
    if chkc then return chkc:IsLocation(LOCATION_GRAVE) and chkc:IsControler(1-tp) and s.filter(chkc,e,tp) end
    if chk==0 then return Duel.IsExistingTarget(s.filter,tp,0,LOCATION_GRAVE,1,nil,e,tp) end
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_TARGET)
    local g=Duel.SelectTarget(tp,s.filter,tp,0,LOCATION_GRAVE,1,1,nil,e,tp)
    Duel.SetOperationInfo(0,CATEGORY_SPECIAL_SUMMON,g,1,0,0)
    Duel.SetOperationInfo(0,CATEGORY_REMOVE,g,1,0,0)
end

function s.activate(e,tp,eg,ep,ev,re,r,rp)
    local tc=Duel.GetFirstTarget()
    if not tc or not tc:IsRelateToEffect(e) then return end

    -- Check available valid paths based on zone availability for option 1
    local b1 = Duel.GetLocationCount(tp,LOCATION_MZONE)>0 and tc:IsCanBeSpecialSummoned(e,0,tp,false,false)
    local b2 = tc:IsAbleToRemove()

    if not b1 and not b2 then return end

    local op=0
    -- The opponent chooses which effect applies. In YGOPro, 1-tp is the opponent making the choice prompt.
    if b1 and b2 then
        op=Duel.SelectOption(1-tp,aux.Stringid(id,0),aux.Stringid(id,1))
    elseif b1 then
        op=Duel.SelectOption(1-tp,aux.Stringid(id,0))
    else
        op=Duel.SelectOption(1-tp,aux.Stringid(id,1))+1
    end

    if op==0 then
        -- Option 1: Special Summon to your field
        Duel.SpecialSummon(tc,0,tp,tp,false,false,POS_FACEUP)
    else
        -- Option 2: Gain LP equal to ATK, then banish
        local atk = math.max(tc:GetAttack(),0)
        Duel.Recover(tp,atk,REASON_EFFECT)
        Duel.Remove(tc,POS_FACEUP,REASON_EFFECT)
    end
end